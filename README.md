# Decouple it!
Some time ago I've started to think about some concepts that will help me with problems that I have with strongly coupled code.

I've created some libraries that may be useful for you. They're basing on the same concept, to make code less coupled and more manageable. Everything is based on ASP .NET Core Microsoft libraries and aligned with it.

All those concepts are implemented withing two microservices located in Samples directory :)
If you want to use one of this libraries they're available on public Nuget repository.

# Problems and solutions

## 1. Oversized not-intuitive bootstrap classes like Startup
### Problem
I faced the problem with let's call it "imperative" way of managing dependencies. There were always one big bootstrapping file setuping whole application. Sometimes there were projects doing the same thing but for the smaller scale. What if we could register everything in more "distributed" way?
### Solution - GS.DecoupleIt.DependencyInjection.Automatic
```c#
public void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.ScanAssemblyForImplementations(GetType().Assembly);
}
```
And that is it. You don't have to think that is something registered or not. The only thing more that has to be done is to decorate implementation classes (or even interfaces) with lifetime attributes.
```c#
[Singleton]
public class ExampleService { ... }

[Singleton]
public class ISomeServiceInterface { ... }
```

## 2. Mess in configuration
### Problem
Another thing that got my attention was gathering configuration. That was the next time when programmer has to remember to register an Options somewhere with appropriate configuration path. When we look at code we can see that it's already segregated within logical catalogs called namespaces. Why not to use that feature to make configuration more attached to code from it's coming from?
### Solution - GS.DecoupleIt.Options.Automatic
```c#
namespace MyApp.Domain.Repositories
{
    [Configure]
    public class GeneralOptions
    {
        public string ConnectionString { get; set; }
    }
}
```
This example above allow to use this configuration:
```json
{
  "MyApp": {
    "Domain": {
      "Repositories": {
        "General": {
          "ConnectionString": "some connection string"
        }
      }
    }
  }
}
```
If we separate whole configuration that way it become really clear which property is related to which part of our code. Nice :)

## 3. Cross-domain calls and dependencies
### Problem
Sometimes it's not clear where to put some code. It happens mostly when multiple domains resides on the same service, which causes to couple tightly code to propagate "reactions" from one domains to another. For example creating a client have to cause automatical creation of his/her archive. There are two separate domain that has to communicate in "imperative" way, so clients domain instructs archive domain to create new archive for client. What if we can do it another way?
### Solution - GS.DecoupleIt.InternalEvents
Let me introduce first some concepts that help you to go with what I've prepared:
- **event** - single event emitted from our code
- **scope** - a part of code that is surrounded by event dispatcher which listens to emitted events and runs appropriate event handlers
- **event handler** - class that handles an event

The main concept is to register all event handlers in dependency injection container and surround code that emits events that all those can be dispatched to handlers.

There are three types of event handlers:
1. **On emission event handler** - an event handler that is run by `Emit()` method of an event. This type of event handler is great to use current transaction to modify additional elements that are located in the same database and write can use already opened transaction. For example created client triggers event handler in history domain, which adds new history entry for client without referencing history code from client code. Simple :) The example below:

```c#
namespace App.Domains.Clients
{
  public class ClientRepository
  {
    public Client CreateClient()
    {
      // ... creation of the client

      new ClientCreated(/*all required data*/).Emit(); // this line runs ClientCreatedOnEmissionHandler
    }
  }
}

namespace App.Domain.Archive
{
  public class ClientCreatedOnEmissionHandler : OnEmissionEventHandlerBase<ClientCreated>
  {
    public override Task HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
    {
      // create history entry
    }
  }
}
```
2. **On success event handler** - this type of an event handler can be used to propagate inforation to external systems when database transaction successfully commits all changes. For example it can be used to transalate internal events to external events using for ex. Kafka or call external service to inform about what have happened. What is great in this concept, event handlers can be outside domain project, so concrete deployment project can define event handler which specifically comminicates with client's system API, which not applies to all clients that we have. The example below assumes that we're using AspNetCore middleware creating event scope for us for every request (available from nugget package).
```c#
namespace App.Deployment.OurClient.Controllers
{
  [Route("clients")]
  public class ClientsController
  {
    private readonly ClientRepository _repository;

    public ClientsController(ClientRepository repository)
    {
      _repository = repository;
    }

    [Post]
    public void CreateNewClient()
    {
      _repository.CreateClient();
    }
  }
}

namespace App.Domains.Clients
{
  public class ClientRepository
  {
    public Client CreateClient()
    {
      // ... creation of the client

      new ClientCreated(/*all required data*/).Emit(); // this line runs ClientCreatedOnSuccessHandler
    }
  }
}

namespace App.Deployment.OurClient.InternalEventHandlers
{
  public class ClientCreatedOnSuccessHandler : OnSuccessEventHandlerBase<ClientCreated>
  {
    public override Task HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
    {
      // publish Kafka event
    }
  }
}
```
`ClientCreatedOnSuccessHandler` will be triggered when request ends normally not by exception.

3. **On failure event handler** - the example above describes perfectly the positive flow. This one works the same way, but with one difference. Event handler will be triggered when action of the controller throws an exception. As previous it can be used to inform external services about failure in our code.

## Complex transaction management
### Problem
I've struggled with it when controller action called some services, those services called another services and so on. I had to pass unit of work object to ensure transaction within all levels of execution of code underneath. Possible simple fix was to use container scoped unit of work, but the code that I had to work with, does not allowed it, because transaction management was implemented case by case in services, command handlers etc. Then I came to another idea...
### Solution - GS.DecoupleIt.Contextual.UnitOfWork
This library has an implementation of generic unit of work accessor that bases on async flow context. It creates a scope that defines boundaries of transaction in code. When any code asks for the same type of unit of work object, it gets the same instance every time when code is run within single async flow. It works perfectly with **On emission event handler** to make some additional work on the same transaction in different domain but on the same database schema for example.
#### Simple usage:
```c#
public class ClientRepository : IClientRepository
{
  public ClientRepository(IUnitOfWorkAccessor accessor)
  {
    _accessor = accessor;
  }

  public async Task AddAsync(Client client)
  {
    await using var context = _accessor.Get<ClientsDbContext>();

    await context.AddAsync(client);
  }

  private readonly IUnitOfWorkAccessor _accessor;
}
```
#### More complex usage:
```c#
public class ClientService
{
  public ClientRepository(IUnitOfWorkAccessor accessor, ClientRepository repository)
  {
    _accessor = accessor;
    _repository = repository;
  }

  // This method is transaction owner.
  public async Task AddIfNotExistsAsync(Guid id)
  {
    await using var context = _accessor.Get<ClientsDbContext>(); // open trasaction

    if (!await context.DoesExists(id))
      await context.AddAsync();

    await context.SaveChanges();
  } // close transaction

  private readonly IUnitOfWorkAccessor _accessor;
  private readonly ClientRepository _repository;
}

// This class is transaction agnostinc. Every method of this class gets the context from higher level. It that context does not exists (in this example does), new unit of work is opened by every call to one of this methods.
public class ClientRepository : IClientRepository
{
  public ClientRepository(IUnitOfWorkAccessor accessor)
  {
    _accessor = accessor;
  }

  public async Task AddAsync(Client client)
  {
    await using var context = _accessor.Get<ClientsDbContext>(); // get context from higher level

    await context.AddAsync(client);
  }

  public async Task<bool> DoesExist(Guid id)
  {
    await using var context = _accessor.Get<ClientsDbContext>(); // get context from higher level

    return context.Clients.AnyAsync(x => x.Id == id);
  }

  private readonly IUnitOfWorkAccessor _accessor;
}
```
It can be seen that there no passing unit of work through parameters of methods and every method can open it's own transaction if such was not created yet. It gives our code autonomy of being used within existing transaction or as methods with its' own transactions.
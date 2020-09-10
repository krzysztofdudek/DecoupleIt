# Decouple it!
Some time ago I've started to think about some concepts that will help me with problems that I have with strongly coupled code.

I've created some libraries that may be useful for you. They're basing on the same concept, to make code less coupled and more manageable. Everything is based on ASP .NET Core Microsoft libraries and aligned with it.

All those concepts are implemented withing two microservices located in Samples directory :)
If you want to use one of this libraries they're on public Nuget repository.

That's what have born from this thoughts:
1. automatic registration of dependencies using **ServiceCollection**
2. automatic gathering of configuration using **Options**
3. implementation of Optionals based on Zoran Horvat implementation from his videos (thank you for the inspiration 😉)
4. internal events (probably the name could be better and more readable)
5. async flow contextual unit of works
6. distributed tracing library using default Microsoft logging library
7. HTTP v1 transparency of usage using RestSharp
8. ASP .NET Core base library integrating core concepts that microservice should have

## 1. GS.DecoupleIt.DependencyInjection.Automatic
I faced the problem with let's call it "imperative" way of managing dependencies. There were always one big bootstrapping file setuping whole application. Sometimes there were projects doing the same thing but for the smaller scale. What if we could register everything in more "distributed" way?

```c#
public void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.ScanAssemblyForImplementations(GetType().Assembly);
}
```

And that is it. That's all. You don't have to think is something registered or not. The only thing more that has to be done is to decorate implementation classes (or even interfaces) with lifetime attributes.
```c#
[Singleton]
public class ExampleService { ... }

[Singleton]
public class ISomeServiceInterface { ... }
```

## 2. GS.DecoupleIt.Options.Automatic
Another thing that got my attention was gathering configuration. That was the next time when programmer has to remember to register an Options somewhere with appropriate configuration path. When we look at code we can see that it's already segregated within logical catalogs called namespaces. Why not to use that feature to make configuration more attached to code from it's coming from?

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

## Rest of the description is in preparation...

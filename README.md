# CED
Configurable Event Dispatcher for .NET

This library is written in .NET Standard 2.0 and should work with .NET Framework 4.6.1 and higher as well as  .NET Core 2 and higher. The test project is targeted at .NET Framework 4.6.1 as a proof of baseline compatability.

# Objective
The goal of this library is to allow config-based setting hooks between .NET Events and Handlers.
Traditionally one would hardcode the handlers to specific events:

```
public event EventHandler<string> SomethingHappened;

...

someObject.SomethingHappened += HandlerMethod;
```

this however creates relatively tight coupling between the classes, at least in consumer -> producer direction. Additionally any changes to what listens to what event required recompilation.

EDC provides an ability to add a config section to your solution that will define how various classes should subscribe to events. This allows for no-recompilation-needed changes, (in the future even runtime changes), as well as using single compiled application with different sets of configuration.

# Basic Usage

1. Register EDC config section (`CED.Config.EventConfigSection`):
```
  <configSections>
    <section name="eventConfig" type="CED.Config.EventConfigSection, CED"/>
  </configSections>
  ```
  
  2. Add config section
  ```
  <eventConfig throwOnErrors="false">
    <events>
    
      <event>
        <producer qualifiedClassName="CED.Tests._461.Producer, CED.Tests.461" eventName="NoDataEvent" />
        <consumers>
          <consumer qualifiedClassName="CED.Tests._461.Consumer, CED.Tests.461" methodName="TriggerNoData" />
        </consumers>
      </event>
      
    </events>
  </eventConfig>
  ```
  Please note, that while `<event>` expects a single `<producer />`, it can contain any number of `<consumer />` entries under `<consumers />` container.
  
  3. Create EventDispatcher. You have a choice of using any of the three constructors:
  ```
  public EventDispatcher(Func<Type, object> resolver = null) // use default "eventConfig" section name, loaded from currenct config
  public EventDispatcher(string configSectionName, Func<Type, object> resolver = null) // use whatever section name you have chosen in current config
  public EventDispatcher(EventConfigSection eventConfigSection, Func<Type,object> resolver = null) // load the section yourself and pass it to the constructor
  ```
  The resolver is optional, but usually needed, if the event producers and consumers are not static. See "Resolution" section below.
  
  4. Call `HookAll()` method. This will apply all loaded consumers to their respective consumers.
  
  That's it - all hooks should be applied, and when events are triggered, defined consumer methods should be called.
  
# Configuration
## eventConfig  
`<eventConfig throwOnErrors="true">`
Contains single `<events />` container.
* `throwOnErrors` (default `true`). If this is set to false, the library should not throw exceptions. This means that setting up the hooks might fail partially or completely.

## events
Contains `<event>` entries. A single event should have it's own entry.
No configurable options available.

## event
Contains a single `<producer />` and single `<consumers />` elements.
No configurable options available.

## producer
`<producer qualifiedClassName="Namespace.Class, Assembly" eventName="EventName" findInstance="true" />`
* `qualifiedClassName` - a fully qualified name of the class or interface that contains the event
* `eventName` - name of the event
* `findInstance` (default: `true`) - whether to use the resolver to find an instance, or treat this as a static event

## consumers
Contains zero or more `<consumer />` entries.
No configurable options available

## consumer
`<consumer qualifiedClassName="Namespace.Class, Assembly" methodName="MethodName" findInstance="true" />`
* `qualifiedClassName` - a fully qualified name of the class or interface that contains the handler method
* `methodName` - name of the method that should be used as event handler
* `findInstance` (default: `true`) - whether to use the resolver to find an instance, or treat this as a static event

# Resolution
Unless the event or handler are static, the library needs a way to resolve a fully qualified type name to an instance. On small scale this can be used simply by creating a method manually:
```
public object Resolver(Type type){
  if(type == typeof(MyType1)
    return instanceOfMyType1;
    ...
}
```
though in most scenarios the method will be wrapping a Dependency Resolver from one of the containers like Autofac:
```
var resolver = new Func<Type, object>(type => DependencyResolver.Current.GetService(type));
```

# Planned development
1. Remove dependency on NLog
2. Provide better feedback to the user about possible failures and results of calling `Hook()` and `Unhook()`
3. Test and fix if necessary loading the resolver from config (undocumented child element of `<eventConfig />` called `<locator qualifiedClassName="Namespace.Class, Assembly" methodName="MethodName" />`, this needs to be static. This removes the necessity to pass resolver manually from code.
4. Improve unit test coverage, specifically for negative scenarios.

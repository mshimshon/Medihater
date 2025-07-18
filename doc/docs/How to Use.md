---
slug: how-to-use
title: How to Use
tags: [blazor, dependency-injection, MediHater, component-patterns, scoped, transient, .net, csharp]
sidebar_position: 3
---

## 🚀 Get Started
Use the same patterns you know from MediatR, but with Mediahater interfaces and classes.

For Full Package:
```bash
dotnet add package Mediahater
```

For Abstractive-only Package:
```bash
dotnet add package Mediahater.Abstractions
```

### Migration from MediatR
Unless you use Pipleline features from MediatR you should be able to substitute their code for ours.

   > **Do NOT** do this on your main branch. Always back up your project or work in a separate branch.

Search for all ```MediatR.``` adn replace with ```Medihater.```.

Search for all ```IMediator``` and replace with ```IMedihator```. 

### Register Services
Mediahater uses a different pattern when it comes the service registration.
#### Manual Registration
```csharp
    services.AddMedihaterServices();
    // With Return Value
    services.AddMedihaterRequestHandler<GetArticleQuery, GetArticleHandler, ArticleResponse>();
    services.AddMedihaterRequestHandler<CreateArticleCommand, CreateArticleHandler>();

    // Add Notifications
    services.AddMedihaterNotificationHandler<SpreadMeNotification, SpreadListennerOneHandler>();
    services.AddMedihaterNotificationHandler<SpreadMeNotification, SpreadListennerTwoHandler>();
```

#### Auto Scan
```csharp
    services.AddMedihaterServices(cfg =>
    {
        cfg.AssembliesScan = [
            typeof(Program)
        ];
    });
```

## Request/Handlers
### Void Task Request
```csharp
public record CreateArticleCommand : IRequest
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
}

```
```csharp

internal class CreateArticleHandler : IRequestHandler<CreateArticleCommand>
{
    public async Task Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        await Task.Delay(100);
    }
}
```
### Request Task with result

```csharp
public record ArticleResponse
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
}
```

```csharp
public record GetArticleQuery(string Id) : IRequest<ArticleResponse>
{

}
```

```csharp
internal class GetArticleHandler : IRequestHandler<GetArticleQuery, ArticleResponse>
{
    public async Task<ArticleResponse> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(500);

        return new ArticleResponse()
        {
            Description = "My Description",
            Title = "The Title"
        };
    }
}
```

### Send the Request
Use the usual ```IMedihator.Send``` to call handler.

Sending without Data Resukt:
```csharp
    _medihater = _serviceProvider.GetRequiredService<IMedihater>();
    var command = new CreateArticleCommand() {
        Title = "",
        Description = ""
    };
    await _medihater.Send(command);
```

Sending wiht Data Result:

```csharp
    _medihater = _serviceProvider.GetRequiredService<IMedihater>();
    var response = await _medihater.Send(new GetArticleQuery("dsad"));
```

## Notifications
### Define the Notification
```csharp
public record SpreadMeNotification(string Message) : INotification
{
}
```
```csharp
internal class SpreadListennerOneHandler : INotificationHandler<SpreadMeNotification>
{
    public async Task Handle(SpreadMeNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(500);
        // my first hook
    }
}
```
```csharp
internal class SpreadListennerOneHandler : INotificationHandler<SpreadMeNotification>
{
    public async Task Handle(SpreadMeNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(500);
        // My second hook
    }
}

### Publish Notification
Use the usual ```IMedihator.Publish``` to call handler.

```csharp
    _medihater = _serviceProvider.GetRequiredService<IMedihater>();
    var notification = new SpreadMeNotification("My Message");
    await _medihater.Publish(notification); 
```

## Performance & Customization

There are several ways you can optimize and control how the pipeline behaves in **Mediahater**.

Out of the box, **Mediahater** is highly optimized. During development, we went from being **5.65× slower** than MediatR to approximately **1.16× faster** than MediatR for equivalent dispatch operations.

> I’m not a benchmarking guru, but the results look very promising.

### How the Pipeline Works

- We use **interface-based middleware** to pass down events.
- Middlewares are intended to be used as **auditors only**.


### Customize Your Pipeline

You have options to define how the pipeline behaves:

- Set **PipelineNotificationMiddleware** strategy:
  - `NeverAwaitMiddlewares` 
  - `OnlyAwaitOrderedExecuted` (Default)
  - `AwaitAllMiddlewares`
- Choose how publish middleware interacts with handlers:
  - `WaitMiddleBeforePulish_To_TriggerNotificationHandlers`
  - `TriggerNotificationHandler_Then_AwaitForBoth` (Default)
- Control request and notification flow.
- Select invocation strategies based on your performance needs.


### Publish Middleware Strategies

#### `WaitMiddleBeforePulish_To_TriggerNotificationHandlers`
- Executes `IPublishMiddleware.BeforePublish` and **awaits** all middlewares before publishing notifications.
- Guarantees middlewares complete **before** any handlers execute.
- Good for strict ordering and consistency.

#### `TriggerNotificationHandler_Then_AwaitForBoth` (Default)
- Runs all middlewares, then notifications, then awaits everything together.
- **No guarantee** that middleware completes before its associated handler executes.
- Higher concurrency, less consistency.

> **Note:** The `PipelineNotificationMiddleware` setting affects this behavior.

### Notification Publish Strategies

#### `NotificationFireMode = PipelineNotificationFireMode.FireAndForget` (Default)
- Publish Notifications if it fails you don't care.
#### `NotificationFireMode = PipelineNotificationFireMode.FireAllAndAwait`
- Publish Notification to all related handlers and await for all to finish. 
#### `NotificationFireMode = PipelineNotificationFireMode.FireOneAndAwait`
- Publish Notification to each of the related handlers and await. 


### Pipeline Notification Middleware Options

#### `NeverAwaitMiddlewares`
- Fire-and-forget — does **not await** middlewares.
- Can cause inconsistencies such as `AfterPublish` triggering before `BeforePublish`.
- Best for maximum speed, parallel scenarios.

#### `OnlyAwaitOrderedExecuted` (Default)
- Awaits **ordered execution**, guaranteeing `BeforePublish` completes before publishing starts.
- Balanced performance and consistency.

#### `AwaitAllMiddlewares`
- Awaits **everything** (Before + After + all notifications).
- Highest consistency, but adds overhead.
- The more middlewares, the slower it gets.

### Middleware Settings
```csharp
    services.AddMedihaterServices(o =>
    {
        o.MiddlewareAwaitMode = Configuraions.Enums.PipelineMiddlewareWaitMode.NeverAwaitMiddlewares;
        o.NotificationMiddleware = Configuraions.Enums.PipelineNotificationMiddleware.TriggerNotificationHandler_Then_AwaitForBoth;
    });
```

### Caching Settings
#### `EagerCaching`
- Cache all at startup
- Heavy traffic or low-latency APIs where every millisecond matters.
- Systems where handlers are stable and known at startup.

#### `LazyCaching` (Default)
- Cache as you go.
- Apps with many handlers but rarely hit all of them.
- Low/medium traffic environments where startup time matters more than first-call performance.

```csharp
    services.AddMedihaterServices(o =>
    {
        o.CachingMode = Configuraions.Enums.PipelineCachingMode.LazyCaching;
    });
```


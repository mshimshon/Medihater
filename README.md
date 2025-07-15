# Mediahater

**Mediahater** is a lightweight, extensible messaging pipeline for .NET developers 
— built to give you full control over request/response dispatching, fire-and-forget notifications, and middleware composition.

After releasing [SwizzleV](https://github.com/mshimshon/SwizzleV), [StatePulse.NET](https://github.com/mshimshon/StatePulse.Net), and [CoreMap](https://github.com/mshimshon/CoreMap), 
it felt natural to close my release spree with a **clearly Licensed,  MediatR alternative**.

No janky license games & No lock-in.

Just a sharp tool under the **forever MIT License**.

## ✨ What You Get

- **Request Handling**
  - Void requests (commands)
  - Response-producing requests (commands/queries)
- **Notification Publishing**
  - Fire-and-forget messages
  - Multiple handlers per notification
- **Middleware Pipelines**
  - Request middlewares (`IRequestMiddleware`)
  - Notification middlewares (`IPublisherMiddleware`)
- **Decouple**
- **Minimal dependencies**
- **No licensing drama**


## 🧭 Why Mediahater?

Some packages start simple, but over time get tangled in fuzzy licensing, dual-licenses, or quiet EULA shifts. Mediahater is here to be:

- **Transparent** — MIT now, MIT forever
- **Simple** — One purpose: in-process message handling
- **Composable** — Extend it, wrap it, or replace parts easily

If you're already using SwizzleV, StatePulse, or CoreMap, this fits right in — a natural continuation for CQRS-like pipelines.

## ❓ Why not use MediatR fork?

There are already well-known mediator packages out there — some powerful, some battle-tested. I won't try to convince you that **Mediahater** does something magical you couldn't write yourself in few lines of code.

But here's the thing:

> It felt right to round off the stack with a clean, reliable messaging system — one thing I know for certain:

>There will be none of team lead tapping me on the shoulder saying:
>*“Hey... this package isn't free anymore. We need to stop updating or start migrating.”*

That’s the entire point of **Mediahater**.

It’s not trying to outdo the giants. It’s here because sometimes, **trust and control matter more than hype**.

If you're already using SwizzleV or StatePulse, this is the missing glue — familiar, safe, and yours to build on without second-guessing.

## 🔏 License

MIT. No weird clauses. No future surprises. You own what you build.

## 🙌 Contributions Welcome

Feel free to open issues, suggest improvements, or submit pull requests. If you like it, star it.


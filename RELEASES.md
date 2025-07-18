## 🔖 Versioning Policy

### 🚧 Pre-1.0.0 (`0.x.x`)

- The project is considered **Work In Progress**.
- **Breaking changes can occur at any time** without notice.
- No guarantees are made about stability or upgrade paths.

### ✅ Post-1.0.0 (`1.x.x` and beyond)

Follows a common-sense semantic versioning pattern:

- **Major (`X.0.0`)**  
  
  - Introduces major features or architectural changes  
  - May include well documented **breaking changes**

- **Minor (`1.X.0`)**  
  
  - Adds new features or enhancements  
  - May include significant bug fixes  
  - **No breaking changes**

- **Patch (`1.0.X`)**  
  
  - Hotfixes or urgent bug fixes  
  - Safe to upgrade  
  - **No breaking changes**
	- 
### v0.9.5 (Initial Release)
#### Features
- ✅ Request/Response handling with `IRequestHandler<TRequest, TResponse>`
- ✅ Notification publishing with `INotificationHandler<TNotification>`
- ✅ Middleware pipelines:
  - `IPublisherMiddleware` for notifications
- ✅ High-performance dispatch (IL-based or cached delegates)
- ✅ Minimal allocations (optimized for speed)
- ✅ Simple DI integration with `IServiceProvider`
- ✅ Ordered or parallel notification execution
- ✅ True-Minimal dependencies
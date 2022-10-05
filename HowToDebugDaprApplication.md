# How to debug Dapr enabled application

## Without Docker

Project Tye can be used. To be able to debug startup `--debug` flag has to be used. This way Tye waits for the debbuger to be attached. It's also possible not to use `--debug` flag, however, code would be executed without waiting for the debugger so it's not possible to debug e.g. startup code.

Tye CLI allows to use `--docker` flag, but it ignores custom dockerfiles. I'm not aware of any way to debug docker with Tye if one wants to use customized dockerfile.

# Docker-compose

**Application cannot be published as a self-contained app to be debbugable - applies both to Rider and VSCode**

Conditional logic has to be used in dockerfile to use `aspnet` instead of `runtime-deps` image as a base and app has to be published not as a self-contained to be able to debug an application in docker container.

Entry point also has to be changed for debugging (`ENTRYPOINT ["./appname"]` for self-contained apps, `ENTRYPOINT ["dotnet", "./appname.dll"]` for _normal_ ones)

## Rider

Rider support debbuging Docker out-of-the-box, however, it's not possible to debug Alpine-based images yet (https://youtrack.jetbrains.com/issue/RIDER-19133 - this should be fixed in Rider 2022).
There's no need to manually attach the debugger, it's posible to debug startup code without any code changes.
`docker-compose.yml` doesn't need to have any volumes with the debbuger mounted - it means that to debug this project with rider
`docker-compose.debug.yml` isn't required.
As for now debbuging works only with `attach: none` configuration.

## VSCode

VSCode allows Alpine-based images to be debbuged, nevertheless startup code can't be debugged without code changes.
A workaround is programatically waiting for the debugger to be attached:

```csharp
public static void Main(string[] args)
        {
            while (!Debugger.IsAttached)
            {
                Console.WriteLine("Waiting for debbuger to be attached");
                Thread.Sleep(300);
            }
            CreateHostBuilder(args).Build().Run();
        }
```

To avoid having to manually confirm copying and attaching the debugger two steps have to be performed:

- Debugger has to be mounted in `docker-compose.yaml`:
  ```yaml
  volumes:
    - ~/.vsdbg:/remote_debugger:rw
  ```
- Launch configuration has to point to the mounted debbuger:
  ```json
  {
    "name": "Docker .NET Core Attach (Preview)",
    "type": "docker",
    "request": "attach",
    "postDebugTask": "docker-compose down: debug",
    "platform": "netCore",
    "containerName": "myapp",
    "netCore": {
      "debuggerPath": "/remote_debugger/vsdbg"
    },
    "sourceFileMap": {
      "/src": "${workspaceFolder}"
    }
  }
  ```

# Summary

|        | container | Alpine images |     debug startup     | self-contained apps |
| :----: | :-------: | :-----------: | :-------------------: | :-----------------: |
|  Tye   |    no     |      yes      |          yes          |         no          |
| Rider  |    yes    |   no (WIP)    |          yes          |         no          |
| VSCode |    yes    |      yes      | code changes required |         no          |
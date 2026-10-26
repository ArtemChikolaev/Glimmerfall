# Валидатор реплеев (ADR 0002, BACKLOG G1.7): тот же Sim.dll, что у игры, в контейнере.
# Читает реплей из stdin, печатает JSON-вердикт строкой; аргументы после имени образа дописываются к verify.
#   docker build -f validator/Dockerfile -t glimmerfall-validator .
#   docker run --rm -i glimmerfall-validator < sim.tests/golden/kiter-short-act.gfr
#   docker run --rm -i glimmerfall-validator --daily 2026-10-26 < daily.gfr
# Теги образов закреплены под global.json (.NET SDK 10.0.4xx) и рантайм 10.0.14.
FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY sim/ sim/
COPY sim.cli/ sim.cli/
RUN dotnet publish sim.cli/Glimmerfall.Sim.Cli.csproj -c Release -o /out --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0.14-noble
WORKDIR /app
COPY --from=build /out .
USER app
ENTRYPOINT ["dotnet", "glimmerfall-sim.dll", "verify", "--json", "-"]

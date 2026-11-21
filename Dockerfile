# Đảo Đèn Lồng — the Test Contract loop on a clean Linux machine (TICKET-04: "commands must run
# on macOS/Linux after build_cmd; Docker allowed"). The deterministic core is pure C#, so this
# image runs the exact graded commands with no Unity license needed:
#
#   docker build -t dao-den-long .
#   docker run --rm dao-den-long
#
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble

WORKDIR /app
COPY . .

RUN dotnet build DotNet/DaoDenLong.Headless -c Release

CMD ["sh", "-c", "\
dotnet run --project DotNet/DaoDenLong.Tests && \
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --bench --seed 42 --duration 30 --out bench.json && \
dotnet run --project DotNet/DaoDenLong.Headless -c Release -- --soak --seed 7 --nights 10 --out soak.json && \
cat bench.json soak.json"]

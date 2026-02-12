FROM mono:latest
WORKDIR /app

RUN sed -i 's|deb.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i 's|security.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i '/buster-updates/d' /etc/apt/sources.list \
 && apt-get update \
 && apt-get install -y mono-xsp4 wget unzip \
 && rm -rf /var/lib/apt/lists/*

RUN wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -O /usr/local/bin/nuget.exe \
 && echo '#!/bin/bash\nmono /usr/local/bin/nuget.exe "$@"' > /usr/local/bin/nuget \
 && chmod +x /usr/local/bin/nuget

RUN mkdir -p /app/bin

RUN nuget install Newtonsoft.Json -OutputDirectory /app/packages \
 && cp /app/packages/Newtonsoft.Json.*/lib/net45/Newtonsoft.Json.dll /app/bin/ || \
    cp /app/packages/Newtonsoft.Json.*/lib/net40/Newtonsoft.Json.dll /app/bin/

COPY . /app
RUN ls -la /app/bin/

EXPOSE 8080
CMD ["sh", "-c", "xsp4 --port ${PORT:-8080} --nonstop --verbose"]

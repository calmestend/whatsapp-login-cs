FROM mono:latest
WORKDIR /app

RUN sed -i 's|deb.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i 's|security.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i '/buster-updates/d' /etc/apt/sources.list \
 && apt-get update \
 && apt-get install -y mono-xsp4 wget unzip \
 && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/bin

# Descargar e instalar Newtonsoft.Json
RUN wget https://www.nuget.org/api/v2/package/Newtonsoft.Json/13.0.3 -O /tmp/newtonsoft.zip && \
    unzip -q /tmp/newtonsoft.zip -d /tmp/newtonsoft && \
    cp /tmp/newtonsoft/lib/net45/Newtonsoft.Json.dll /app/bin/Newtonsoft.Json.dll && \
    rm -rf /tmp/newtonsoft /tmp/newtonsoft.zip

COPY . /app

RUN ls -la /app/bin/

EXPOSE 8080
CMD ["sh", "-c", "xsp4 --port ${PORT:-8080} --nonstop --verbose"]

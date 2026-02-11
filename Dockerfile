FROM mono:latest

WORKDIR /app
COPY . /app

RUN sed -i 's|deb.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i 's|security.debian.org|archive.debian.org|g' /etc/apt/sources.list \
 && sed -i '/buster-updates/d' /etc/apt/sources.list \
 && apt-get update \
 && apt-get install -y mono-xsp4 \
 && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
CMD ["xsp4", "--port", "8080", "--nonstop"]


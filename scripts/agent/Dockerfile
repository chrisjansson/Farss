FROM ubuntu:23.10

RUN apt update && apt upgrade -y && apt install -y wget
RUN wget https://github.com/nushell/nushell/releases/download/0.94.2/nu-0.94.2-x86_64-unknown-linux-gnu.tar.gz 
RUN tar -xvzf nu-0.94.2-x86_64-unknown-linux-gnu.tar.gz
RUN cp /nu-0.94.2-x86_64-unknown-linux-gnu/nu /usr/bin/nu

COPY config.nu /root/.config/nushell/
COPY env.nu /root/.config/nushell/

RUN apt install -y dotnet-sdk-8.0
RUN wget "https://s3.dualstack.us-east-1.amazonaws.com/gitlab-runner-downloads/latest/deb/gitlab-runner_amd64.deb"
RUN apt install -y git curl
RUN dpkg -i gitlab-runner_amd64.deb 

ENV NODE_VERSION=20.14.0
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
ENV NVM_DIR=/root/.nvm
RUN . "$NVM_DIR/nvm.sh" && nvm install ${NODE_VERSION}
RUN . "$NVM_DIR/nvm.sh" && nvm use v${NODE_VERSION}
RUN . "$NVM_DIR/nvm.sh" && nvm alias default v${NODE_VERSION}
ENV PATH="/root/.nvm/versions/node/v${NODE_VERSION}/bin/:${PATH}"
RUN corepack enable
RUN apt install -y docker.io
ENTRYPOINT ["gitlab-runner"]
CMD ["run"]

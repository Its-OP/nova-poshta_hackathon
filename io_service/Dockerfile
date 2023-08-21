# Use a base image that supports your desired Go version
FROM golang:1.20


# Download and install OpenSSL
RUN wget -O - https://www.openssl.org/source/openssl-1.1.1u.tar.gz | tar zxf - \
    && cd openssl-1.1.1u \
    && ./config --prefix=/usr/local \
    && make -j$(nproc) \
    && make install_sw install_ssldirs \
    && ldconfig -v

# Set ARGironment variables for OpenSSL and SpeechSDK
ARG SSL_CERT_DIR="/etc/ssl/certs"
ARG LD_LIBRARY_PATH="/usr/local/lib:$LD_LIBRARY_PATH"

# Install necessary dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    libssl-dev \
    ca-certificates \
    libasound2 \
    wget

ARG SPEECHSDK_ROOT="/speechsdk"

# Download and install Speech SDK
RUN mkdir -p "$SPEECHSDK_ROOT" \
    && wget -O SpeechSDK-Linux.tar.gz https://aka.ms/csspeech/linuxbinary \
    && tar --strip 1 -xzf SpeechSDK-Linux.tar.gz -C "$SPEECHSDK_ROOT" \
    && rm SpeechSDK-Linux.tar.gz

ARG ARCHITECTURE="x64"
# Set CGO_CFLAGS, CGO_LDFLAGS, and LD_LIBRARY_PATH
ENV CGO_CFLAGS="-I$SPEECHSDK_ROOT/include/c_api"
ENV CGO_LDFLAGS="-L$SPEECHSDK_ROOT/lib/$ARCHITECTURE -lMicrosoft.CognitiveServices.Speech.core"
ENV LD_LIBRARY_PATH="$SPEECHSDK_ROOT/lib/$ARCHITECTURE:$LD_LIBRARY_PATH"

# Create a working directory for the Go application
WORKDIR /app

# Copy your Go application source code to the container
COPY . .

COPY go.mod go.sum ./
RUN go mod download

# Build the Go application
RUN go build -o ./app ./cmd/assistant/main

# Run the Go application
CMD ["./app"]

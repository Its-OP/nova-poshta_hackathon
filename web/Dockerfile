# Step 1: Build the Vue.js application
FROM node:latest AS build-stage

WORKDIR /app

# Copy package.json and package-lock.json to leverage Docker caching
COPY package*.json ./

# Install dependencies
RUN npm install

# Install Vite globally (if needed)
RUN npm install -g vite

# Copy the rest of the application
COPY . .

# Build the application using Vite
RUN npm run build

# Serve the application using a lightweight server like serve
FROM node:latest AS production-stage

WORKDIR /app

# Install serve globally
RUN npm install -g serve

# Copy the built application from the previous stage
COPY --from=build-stage /app/dist /app

# Expose port 5000 for serve
EXPOSE 5000

# Start serve
CMD ["serve", "-s", "/app", "-l", "5000"]

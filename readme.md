cách chạy docker thủ công:

1> docker build -t demo-api:v1 .    => build docker => file script build: dockerfile
2> docker run -d --name api-v1 -p 5000:8080 demo-api:v1    => build docker, create image, register container and port
3> docker ps     => list image/container running

Bước 1: Build Image
docker build -t demo-api:v1 .
----------------------------------------
Đọc Dockerfile
↓
Build Image
↓
Đặt tên image = demo-api:v1
----------------------------------------
Bước 2: Run Container
docker run -d --name api-v1 -p 5000:8080 demo-api:v1
----------------------------------------
Image demo-api:v1
      ↓
Create Container api-v1
      ↓
Start Container
      ↓
Map Port 5000 -> 8080
----------------------------------------

docker build -t demo-api:v1 . 
=> docker dang đóng gói ứng dụng

khác biệt giữa dotnet run <> docker build

dot net run => build & run project 
Source Code
    ↓
Build
    ↓
Run

docker build     =>  docker run
Source Code             Image
    ↓                     ↓
Build                 Container
    ↓                     ↓
Image                    Run

just build Image only, not run 
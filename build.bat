@echo off
echo [starting push zlearn api to dockerhub...]

REM Bật chế độ kiểm tra lỗi
setlocal EnableDelayedExpansion

REM Xóa image cũ
docker stop zlearn_web
docker rm zlearn_web
docker rmi hoangcndev/zlearn:zlearn_web-1.0
docker rmi zlearn_web:1.0

REM Build image
docker build -t zlearn_web:1.0 . || (
    echo [Error: Failed to build image!]
    exit /b 1
)

REM Tag image
docker tag zlearn_web:1.0 hoangcndev/zlearn:zlearn_web-1.0 || (
    echo [Error: Failed to tag image!]
    exit /b 1
)

REM Push image
docker push hoangcndev/zlearn:zlearn_web-1.0 || (
    echo [Error: Failed to push image!]
    exit /b 1
)

echo [push zlearn web to dockerhub successfully!]
exit /b 0
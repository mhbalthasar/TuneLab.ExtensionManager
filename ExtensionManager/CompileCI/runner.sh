#!/bin/bash

if [ -n "$DOTNET_HOME" ] && [ -x "$DOTNET_HOME/dotnet" ]; then
    DOTNET_PATH="$DOTNET_HOME/dotnet"
elif [ -n "$DOTNET_ROOT" ] && [ -x "$DOTNET_ROOT/dotnet" ]; then
    DOTNET_PATH="$DOTNET_ROOT/dotnet"
else
    DOTNET_PATH=$(which dotnet)
fi

if [ -z "$DOTNET_PATH" ]; then
    echo "Cannot find the path of dotnet，Please check the DOTNET_HOME、DOTNET_ROOT、PATH Environment"
    exit 1
fi

# 获取脚本所在目录
script_dir=$(cd "$(dirname "$0")" && pwd)
script_name=$(basename "$0" .sh)

dll_file="$script_dir/${script_name}.dll"

if [ ! -f "$dll_file" ]; then
    echo "cannot find the dll：$dll_file"
    exit 1
fi

"$DOTNET_PATH" "$dll_file"

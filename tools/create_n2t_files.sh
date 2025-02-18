#!/bin/bash

# Check if a path is provided as an argument
if [ -z "$1" ]; then
  echo "Usage: $0 <path>"
  exit 1
fi

# Extract the directory path and base name from the provided path
dir_path=$(dirname "$1")
base_name=$(basename "$1")

# Create the necessary directories
mkdir -p "$dir_path"

# Change to the target directory
cd "$dir_path" || exit

# Create files with the specified extensions
for ext in hdl tst cmp out; do
  touch "${base_name}.${ext}"
done

echo "Created ${base_name}.hdl, ${base_name}.tst, and ${base_name}.cmp in ${dir_path}"

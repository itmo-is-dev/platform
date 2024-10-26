#!/bin/bash

function csproj_path_to_unix_path {
  cat | tr '\\' '/'
}

function csproj_path_to_short_name {
  while IFS= read -r line
  do
    basename "$(dirname "$line")" | sed 's/Itmo.Dev.Platform.//'
  done
}

function dir_path_to_short_name {
  while IFS= read -r line
  do
    basename "$line" | sed 's/Itmo.Dev.Platform.//'
  done
}

function short_name_to_full_name {
  while read -r line
  do
    echo Itmo.Dev.Platform."$line"
  done
}

function short_name_to_csproj_path {
  while IFS= read -r line
  do
    find . -type f -name Itmo.Dev.Platform."$line".csproj | sed 's/^.\///'
  done
}

function short_name_to_dir_path {
  while IFS= read -r name
  do
    find . -type d -name Itmo.Dev.Platform."$name" | sed 's/^.\///'
  done
}

function get_tag_attribute_value {
  attribute_name="$1"
  cat | grep -Eo "$attribute_name"'[ ]*=[ ]*"[a-zA-Z0-9\.]*"[ ]*' | grep -Eo '".*"' | tr -d '"'
}

function lines_to_json_array {
  cat \
    | jq --raw-input --slurp 'split("\n") | map(select(. != ""))' \
    | tr -d '\n'
}

function json_array_to_lines {
    cat | jq -r 'values[]'
}
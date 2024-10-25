#!/bin/bash

projects=$(find ./src -name '*.csproj' | tr '\n' ' ')
          
for project in ${projects}
do
  packable_count=$(grep -Ec '<IsPackable>true</IsPackable>' "$project")

  if [[ ! "${packable_count}" -eq 0 ]]
  then
    basename "$(dirname "${project}")" | sed 's/Itmo.Dev.Platform.//'
  fi
done
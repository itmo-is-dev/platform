#!/bin/bash

project="$1"
project_references=$(.github/workflows/publish-find-project-references.sh "${project}")

for project_reference in ${project_references}
do
  project_csproj=$(ls "${project_reference}" | grep -E 'csproj$')
  project_csproj="$project_reference"/"$project_csproj"
  
  grep PackageReference "${project_csproj}" | grep -Eo 'Include=".*"' | grep -Eo '".*"' | tr -d '"'
done
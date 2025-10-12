#!/usr/bin/env fish

set file_name 'kanjidic2.xml'
set project_dir (status dirname)

set file_fetch_script \
    "$project_dir"/../Data/edrdg-dictionary-archive/scripts/get_file_by_date.fish

set file_path (
    fish "$file_fetch_script" \
        --latest \
        --file "$file_name")

time dotnet run \
    --project "$project_dir" \
    --configuration Release \
    -- "$file_path"

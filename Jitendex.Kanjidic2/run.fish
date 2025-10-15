#!/usr/bin/env fish
# Copyright (c) 2025 Stephen Kraus
# SPDX-License-Identifier: AGPL-3.0-or-later

set file_name 'kanjidic2.xml'
set project_dir (realpath (status dirname))

set file_fetch_script \
    "$project_dir"/../Data/edrdg-dictionary-archive/scripts/get_file_by_date.fish

set file_path (
    fish "$file_fetch_script" \
        --latest \
        --file "$file_name")

time dotnet run \
    --project "$project_dir" \
    --configuration 'Release' \
    -- "$file_path"

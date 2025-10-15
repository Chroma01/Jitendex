#!/usr/bin/env fish
# Copyright (c) 2025 Stephen Kraus
# SPDX-License-Identifier: AGPL-3.0-or-later

set jmdict_name 'JMdict_e_examp'
set project_dir (realpath (status dirname))

set file_fetch_script \
    "$project_dir"/../Data/edrdg-dictionary-archive/scripts/get_file_by_date.fish

set jmdict_file (
    fish "$file_fetch_script" \
        --latest \
        --file "$jmdict_name")

set xref_ids_file \
    "$project_dir"/../Data/jitendex-data/jmdict/cross_reference_sequences.json

time dotnet run \
    --project "$project_dir" \
    --configuration 'Release' \
    -- "$jmdict_file" \
    --xref-ids "$xref_ids_file"

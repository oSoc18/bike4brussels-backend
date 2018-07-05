#!/bin/bash
export LC_ALL=C
month="$(date --date="$(date +%Y-%m-15) -1 month" +'%B')"
year="$(date --date="$(date +%Y-%m-15) -1 month" +'%Y')"
file="../wwwroot/requests/data/$month-$year.csv"
message="Attached is the monthly overview of the requests to the backend service of the Bike for Brussels Routeplanner.

The requests are bundled in a csv file with 5 columns, containing respectively: timestamp of request, latitude of starting position, longitude of starting position, latitude of ending position, longitude of ending position. The coordinates use the WSG84 coordinate system."
recipients="fdepoortere@gob.brussels swalschap@gob.brussels rcappellen@gob.brussels kdeteme@gob.brussels"

echo "$message" | mailx -r "data-overview@routeplanner.bike.brussels" -s "Data rapport for $month $year" -a "$file" $recipients

library(sf)
library(tidyverse)

usa_crs <- 32610

read_shootings <- function(filepath) {
  read_csv(filepath) %>% 
    st_as_sf(coords=c("Longitude", "Latitude"),crs=4326) %>% 
    st_transform(usa_crs)
}

grid_count <- function(data, cellsize) {
  hexgrid <- data %>% 
    st_make_grid(square=F, cellsize=cellsize) %>% 
    st_sf() %>% 
    rowid_to_column('hex_id')
  
  counts <- st_join(data, hexgrid, join=st_within)
  
  pts_in_hex <- counts %>% 
    st_set_geometry(NULL) %>% 
    count(name='count', hex_id)
  
  hexgrid %>% 
    left_join(pts_in_hex) %>% 
    replace(is.na(.), 0)
}

clip_result <- function(grid, zone) {
  grid %>% st_filter(zone, .predicate = st_within)
}

write_result <- function(grid, filepath, radius) {
  grid %>% 
    st_centroid() %>% 
    st_transform(4326) %>% 
    cbind(., st_coordinates(.)) %>% 
    st_set_geometry(NULL) %>% 
    mutate(radius = radius) %>% 
    dplyr::select(Longitude=X,Latitude=Y, count, radius) %>% 
    write_csv(filepath)
}

RADIUS <- 400

boston_zone <- st_read('C:/Users/macie/Desktop/intouch/boston_zone.geojson')
boston_shootings <- read_shootings('C:/Users/macie/Desktop/intouch/boston_shootings_2015_2020.csv')
boston_counts <- grid_count(boston_shootings, RADIUS)

write_result(
  clip_result(boston_counts, boston_zone),
  'C:/Users/macie/Desktop/intouch/boston_hex_cut.csv',
  RADIUS
)

manhattan_zone <- st_read('C:/Users/macie/Desktop/intouch/manhattan_zone.geojson')
manhattan_shootings <- read_shootings('C:/Users/macie/Desktop/intouch/manhattan_shootings_2015_2020.csv')
manhattan_counts <- grid_count(manhattan_shootings, RADIUS)

write_result(
  clip_result(manhattan_counts, manhattan_zone),
  'C:/Users/macie/Desktop/intouch/manhattan_hex_cut.csv',
  RADIUS
)

brooklyn_zone <- st_read('C:/Users/macie/Desktop/intouch/brooklyn_zone.geojson')
brooklyn_shootings <- read_shootings('C:/Users/macie/Desktop/intouch/brooklyn_shootings_2015_2020.csv')
brooklyn_counts <- grid_count(brooklyn_shootings, RADIUS)

write_result(
  clip_result(brooklyn_counts, brooklyn_zone),
  'C:/Users/macie/Desktop/intouch/brooklyn_hex_cut.csv',
  RADIUS
)

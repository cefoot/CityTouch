library(sf)
library(tidyverse)

# SHOOTINGS DATA
# DATA PERIOD: 2015 - 2022
# CITIES: NYC, BOSTON


# DATA LOAD FUNCTIONS =======

read_new_york_shootings <- function(filepath) {
  csvfile <- read_csv(filepath) %>% 
    mutate(YEAR = as.integer(substring(OCCUR_DATE, 7))) %>% 
    filter(between(YEAR, 2015, 2020)) %>% 
    dplyr::select(INCIDENT_KEY, Latitude, Longitude) %>% 
    st_as_sf(coords=c('Longitude', 'Latitude'), crs=4326)
}

read_boston_shootings <- function(folderpath) {
  filepaths <-  c(2015:2020) %>%
    as.character %>% 
    paste0(folderpath, ., '.csv')
  
  do.call(rbind, lapply(filepaths, FUN = function(x) 
    dplyr::select(read_csv(x), INCIDENT_KEY=INCIDENT_NUMBER, Latitude=Lat, Longitude=Long, SHOOTING)
  )) %>% 
    filter(!is.na(Latitude), !is.na(Longitude), !is.na(SHOOTING), SHOOTING == 'Y') %>% 
    dplyr::select(-SHOOTING) %>% 
    st_as_sf(coords=c('Longitude', 'Latitude'), crs=4326)
}

# DATA FILTERING =====

select_shootings <- function(data, zone) {
  data %>% 
    st_filter(zone) %>% 
    st_transform(4326)
}

# DATA OUTPUT ========

write_output <- function(data, outpath) {
  data %>% 
    cbind(., st_coordinates(.)) %>% 
    rename(Longitude=X, Latitude=Y) %>% 
    st_set_geometry(NULL) %>% 
    write_csv(outpath)
}

# NYC data ====================================

nyc_path <- 'C:/Users/macie/Desktop/intouch/NYPD_Shooting_Incident_Data__Historic_NORMALIZED.csv'

nyc_data <- read_new_york_shootings(nyc_path) %>% 
  st_transform(usa_crs)


# Boston data =================================
boston_folderpath <- 'C:/Users/macie/Desktop/intouch/boston/'
boston_data <- read_boston_shootings(boston_folderpath) %>% 
  st_transform(usa_crs)

# center_point <- c(-71.0602581145983, 42.36084693665502)


# ====== EXECUTE ======

manhattan_zone <- st_read('C:/Users/macie/Desktop/intouch/manhattan_zone.geojson')

write_output(
  select_shootings(
    nyc_data,
    manhattan_zone
  ),
  'C:/Users/macie/Desktop/intouch/manhattan_shootings_2015_2020.csv'
)

brooklyn_zone <- st_read('C:/Users/macie/Desktop/intouch/brooklyn_zone.geojson')
write_output(
  select_shootings(
    nyc_data,
    brooklyn_zone
  ),
  'C:/Users/macie/Desktop/intouch/brooklyn_shootings_2015_2020.csv'
)

boston_zone <- st_read('C:/Users/macie/Desktop/intouch/boston_zone.geojson')
write_output(
  select_shootings(
    boston_data,
    boston_zone
  ),
  'C:/Users/macie/Desktop/intouch/boston_shootings_2015_2020.csv'
)


# 
# output <- spatial %>%
#   st_transform(usa_crs) %>% 
#   st_filter(zone, .pred = intersects) %>%
#   st_transform(4326)
# 
# output %>% 
#   cbind(., st_coordinates(.)) %>%
#   rename(Longitude=X, Latitude=Y) %>% 
#   st_set_geometry(NULL) %>%
#   write_csv()

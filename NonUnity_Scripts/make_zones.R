library(sf)
library(tidverse)

usa_crs <- 32610

make_zone <- function(pt, extent) {
  st_sfc(st_point(pt), crs=4326) %>% 
    st_transform(usa_crs) %>% 
    st_buffer(extent)
}

write_zone <- function(data, filepath) {
  st_write(
    data,
    filepath,
    delete_dsn=TRUE
  )
}

extent <- 2350

manhattan_pt <- c(-73.9941092674557,40.7347250654864)
brooklyn_pt <- c(-73.91639978538994, 40.67182506256504)
boston_pt <- c( -71.05528541435912, 42.36271715074176)

write_zone(
  make_zone(manhattan_pt, extent),
  'C:/Users/macie/Desktop/intouch/manhattan_zone.geojson'
)

write_zone(
  make_zone(brooklyn_pt, extent),
  'C:/Users/macie/Desktop/intouch/brooklyn_zone.geojson'
)

write_zone(
  make_zone(boston_pt, extent),
  'C:/Users/macie/Desktop/intouch/boston_zone.geojson'
)

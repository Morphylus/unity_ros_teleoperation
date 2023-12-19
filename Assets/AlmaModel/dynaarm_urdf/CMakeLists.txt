cmake_minimum_required(VERSION 3.5.1)
project(dynaarm_urdf)

find_package(catkin REQUIRED)

set(PACKAGE_DEPENDENCIES)
find_package(robotiq_2f_model QUIET)

if(robotiq_2f_model_FOUND)
  add_definitions(-D_ROBOTIQ_2F_MODEL_FOUND_)
  set(PACKAGE_DEPENDENCIES
    ${PACKAGE_DEPENDENCIES}
    robotiq_2f_model
  )
endif()

find_package(ezgripper_description QUIET)

if(ezgripper_description_FOUND)
  add_definitions(-D_EZGRIPPER_DESCRIPTION_FOUND_)
  set(PACKAGE_DEPENDENCIES
    ${PACKAGE_DEPENDENCIES}
    ezgripper_description
    )
endif()

find_package(rokubimini_rsl_gazebo_plugin QUIET)

if(rokubimini_rsl_gazebo_plugin_FOUND)
  add_definitions(-D_ROKUBIMINI_RSL_GAZEBO_PLUGIN_FOUND_)
  set(PACKAGE_DEPENDENCIES
    ${PACKAGE_DEPENDENCIES}
    rokubimini_rsl_gazebo_plugin
  )
endif()


foreach(sdk ${PACKAGE_DEPENDENCIES})
  message("Found package: " ${sdk})
endforeach()

catkin_package(
  CATKIN_DEPENDS
   ${PACKAGE_DEPENDENCIES}
)

install(DIRECTORY launch urdf bin meshes
  DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}
  )

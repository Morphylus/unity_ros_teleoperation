#!/bin/bash

# Generate urdf and store it in a file
echo "Generating urdf in $(rospack find alma_rsl_urdf)/resources ..."
rosrun xacro xacro  -o  $(rospack find alma_rsl_urdf)/resources/alma.urdf $(rospack find alma_rsl_urdf)/urdf/alma.urdf.xacro

echo "Done!"

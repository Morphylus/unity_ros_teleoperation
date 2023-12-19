#!/bin/bash

# Generate urdf and store it in a file
echo "Generating urdf in $(rospack find dynaarm_urdf)/bin ..."
rosrun xacro xacro -o $(rospack find dynaarm_urdf)/bin/dynaarm.urdf $(rospack find dynaarm_urdf)/urdf/dynaarm.urdf.xacro

echo "Done!"

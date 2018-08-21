#! /bin/bash

for filename in $(ls $1*.csv); do 
     sed 1d $filename >> $2$(basename $filename);
done

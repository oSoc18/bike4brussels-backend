cd /var/www/bike4brussels-backend
./build.sh
./publish.sh
rm -rf /var/www/b4b-backend/publish_old
mv /var/www/b4b-backend/publish/* /var/www/b4b-backend/publish_old
sudo cp -R src/bin/Release/netcoreapp2.0/rhel.7-x64/publish/ /var/www/b4b-backend/
sudo cp -R src/profiles /var/www/b4b-backend/publish
sudo cp -R src/language /var/www/b4b-backend/publish
sudo cp -R src/scripts /var/www/b4b-backend/publish
sudo cp -R src/wwwroot /var/www/b4b-backend/publish
sudo cp -R src/parkings /var/www/b4b-backend/publish
# sudo mkdir /var/www/b4b-backend/publish/mapdata
sudo cp -R src/mapdata /var/www/b4b-backend/publish
sudo chmod -R 777 /var/www/b4b-backend/publish
sudo systemctl restart b4b-backend.service
echo script done.

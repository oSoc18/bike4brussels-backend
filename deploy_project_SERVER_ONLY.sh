cd /var/www/bike4brussels-backend
./build.sh
./publish.sh
rm -rf /var/www/bike4brussels-backend/publish_old
mv /var/www/bike4brussels-backend/publish/* /var/www/bike4brussels-backend/publish_old
sudo cp -R src/bin/Release/netcoreapp2.0/rhel.7-x64/publish/ /var/www/bike4brussels-backend/
sudo cp -R src/profiles /var/www/bike4brussels-backend/publish
sudo cp -R src/language /var/www/bike4brussels-backend/publish
sudo cp -R src/scripts /var/www/bike4brussels-backend/publish
sudo cp -R src/wwwroot /var/www/bike4brussels-backend/publish
sudo cp -R src/parkings /var/www/bike4brussels-backend/publish
# sudo mkdir /var/www/b4b-backend/publish/mapdata
sudo cp -R src/mapdata /var/www/bike4brussels-backend/publish
sudo chmod -R 777 /var/www/bike4brussels-backend/publish
sudo systemctl restart b4b-backend.service
echo script done.

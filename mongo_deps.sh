
echo "atlass is not installed, installing..."
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
  wget https://fastdl.mongodb.org/mongocli/mongodb-atlas-cli_1.1.1_linux_x86_64.tar.gz
  tar -xzf mongodb-atlas-cli_1.1.1_linux_x86_64.tar.gz
  export PATH=$PATH:mongodb-atlas-cli_1.1.1_linux_x86_64/bin
elif [[ "$OSTYPE" == "darwin"* ]]; then
  brew install mongodb-atlas-cli
fi

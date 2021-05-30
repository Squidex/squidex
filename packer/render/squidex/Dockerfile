FROM squidex/squidex:5

ADD wait.sh /wait.sh

RUN chmod +x /wait.sh

ENTRYPOINT ["/bin/sh", "/wait.sh"]

# UI tests for Cosmos / Squidex

## How to setup the tests

### 1. Install NPM dependencies

    npm i

###  2. Setup protractor

You have to run the following command before every testing session:

    npm run preparee

### 3. Run the frontend server

If you want to run the tests locally with the development version of Squidex you also need to run the webpack server:

    cd ./../src/squidex && npm start


## How to run the tests?

### How to run the tests locally?

    npm test

    // Run them locally with an already running squidex instance.
    npm run test:running

### How to run the tests in the CI?

    // Run them in the CI
    npm run test:ci

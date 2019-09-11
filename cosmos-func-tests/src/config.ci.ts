import { buildConfig } from './config';

// Config to run protractor in the CI
export const config = buildConfig({ url: 'http://staging.cosmos:5000', setup: true });
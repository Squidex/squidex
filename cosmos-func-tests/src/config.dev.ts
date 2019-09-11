import { buildConfig } from './config';

// Config to run protractor locally
export const config = buildConfig({ url: 'http://localhost:5000', setup: true });
/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import 'core-js/es6';
import 'reflect-metadata';

require('zone.js/dist/zone');

if (process.env.ENV !== 'production') {
    Error['stackTraceLimit'] = Infinity;

    require('zone.js/dist/long-stack-trace-zone');
}
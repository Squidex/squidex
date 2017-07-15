/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable, ViewContainerRef } from '@angular/core';

@Injectable()
export class RootViewService {
    public rootView: ViewContainerRef;
}
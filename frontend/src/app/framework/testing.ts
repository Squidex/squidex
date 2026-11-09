/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injector, Provider, runInInjectionContext, Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';

export function createService<T>(type: Type<T>, providers: Provider[]): T {
    // A child injector allows to create multiple instances with different providers in the same test.
    const injector = Injector.create({ providers, parent: TestBed.inject(Injector) });

    return runInInjectionContext(injector, () => new type());
}

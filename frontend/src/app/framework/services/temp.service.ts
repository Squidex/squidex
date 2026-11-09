/*
* Squidex Headless CMS
*
* @license
* Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
*/

import { Service } from '@angular/core';

@Service()
export class TempService {
    private value: any = null;

    public put(value: any) {
        this.value = value;
    }

    public fetch() {
        const result = this.value;

        this.value = null;

        return result;
    }
}

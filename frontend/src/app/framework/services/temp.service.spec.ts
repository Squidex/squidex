/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TempService } from './temp.service';

describe('TempService', () => {
    it('should instantiate', () => {
        const tempService = new TempService();

        expect(tempService).toBeDefined();
    });

    it('should return null if nothing is stored', () => {
        const tempService = new TempService();

        const temp = tempService.fetch();

        expect(temp).toBeNull();
    });

    it('should return value once if something is stored', () => {
        const tempService = new TempService();

        tempService.put('Hello');

        const temp1 = tempService.fetch();
        const temp2 = tempService.fetch();

        expect(temp1).toBe('Hello');
        expect(temp2).toBeNull();
    });
});

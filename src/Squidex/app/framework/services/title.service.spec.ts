/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import {
    TitlesConfig,
    TitleService,
    TitleServiceFactory
} from './title.service';

describe('TitleService', () => {
    beforeEach(() => {
        document.title = '';
    });

    it('should instantiate from factory', () => {
        const titleService = TitleServiceFactory(new TitlesConfig());

        expect(titleService).toBeDefined();
    });

    it('should instantiate', () => {
        const titleService = new TitleService(new TitlesConfig());

        expect(titleService).toBeDefined();
    });

    it('should use single part when title element is pushed', () => {
        const titleService = new TitleService(new TitlesConfig());

        titleService.push('my-title');

        expect(document.title).toBe('my-title');
    });

    it('should concatenate multiple parts when title elements are pushed', () => {
        const titleService = new TitleService(new TitlesConfig());

        titleService.push('my-title1');
        titleService.push('my-title2');

        expect(document.title).toBe('my-title1 | my-title2');
    });

    it('should replace previous element when found', () => {
        const titleService = new TitleService(new TitlesConfig());

        titleService.push('my-title1');
        titleService.push('my-title2');
        titleService.push('my-title3', 'my-title2');

        expect(document.title).toBe('my-title1 | my-title3');
    });

    it('should concatenate remainging parts when title elements are popped', () => {
        const titleService = new TitleService(new TitlesConfig());

        titleService.push('my-title1');
        titleService.pop();

        expect(document.title).toBe('my-title2');
    });

    it('should prepand prefix to title', () => {
        const titleService = new TitleService(new TitlesConfig('prefix'));

        titleService.push('my-title');

        expect(document.title).toBe('prefix - my-title');
    });

    it('should append suffix to title', () => {
        const titleService = new TitleService(new TitlesConfig( undefined, 'suffix'));

        titleService.push('my-title');

        expect(document.title).toBe('my-title - suffix');
    });

    it('should use suffix when stack is empty', () => {
        const titleService = new TitleService(new TitlesConfig('prefix', 'suffix'));

        titleService.pop();

        expect(document.title).toBe('suffix');
    });

    it('should use suffix when stack is empty and no suffix is set', () => {
        const titleService = new TitleService(new TitlesConfig('prefix'));

        titleService.pop();

        expect(document.title).toBe('prefix');
    });
});

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
        const titleService = TitleServiceFactory(new TitlesConfig({}));

        expect(titleService).toBeDefined();
    });

    it('should instantiate', () => {
        const titleService = new TitleService(new TitlesConfig({}));

        expect(titleService).toBeDefined();
    });

    it('should use key when title key is not found in configuration', () => {
        const titleService = new TitleService(new TitlesConfig({}));

        titleService.setTitle('my-title', {});

        expect(document.title).toBe('my-title');
    });

    it('should prepand prefix to title', () => {
        const titleService = new TitleService(new TitlesConfig({}, 'myapp'));

        titleService.setTitle('my-title', {});

        expect(document.title).toBe('myapp - my-title');
    });

    it('should append suffix to title', () => {
        const titleService = new TitleService(new TitlesConfig({}, undefined, 'myapp'));

        titleService.setTitle('my-title', {});

        expect(document.title).toBe('my-title - myapp');
    });

    it('should do nothing if title is null', () => {
        const titleService = new TitleService(new TitlesConfig({}, undefined, 'myapp'));

        titleService.setTitle(null!, {});

        expect(document.title).toBe('');
    });

    it('should set document title when title key is found in configuration', () => {
        const titles: { [key: string]: string } = {
            found: 'found-title'
        };

        const titleService = new TitleService(new TitlesConfig(titles));

        titleService.setTitle('found', {});

        expect(document.title).toBe('found-title');
    });

    it('should set document title to formatted string when title key found in configuration', () => {
        const titles: { [key: string]: string } = {
            found: 'my {value} in title'
        };

        const titleService = new TitleService(new TitlesConfig(titles));

        titleService.setTitle('found', { value: 'secret' });

        expect(document.title).toBe('my secret in title');
    });
});

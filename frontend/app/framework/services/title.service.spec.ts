/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, It, Mock } from 'typemoq';
import { LocalizerService } from './localizer.service';
import { TitlesConfig, TitleService } from './title.service';

describe('TitleService', () => {
    let localizer: IMock<LocalizerService>;

    beforeEach(() => {
        document.title = '';

        localizer = Mock.ofType<LocalizerService>();

        localizer.setup(x => x.getOrKey(It.isAnyString()))
            .returns((key: string) => key.substr(5));
    });

    it('should instantiate', () => {
        const titleService = new TitleService(new TitlesConfig(), localizer.object);

        expect(titleService).toBeDefined();
    });

    it('should use single part if title element is pushed', () => {
        const titleService = new TitleService(new TitlesConfig(), localizer.object);

        titleService.push('i18n:my-title');

        expect(document.title).toBe('my-title');
    });

    it('should concatenate multiple parts if title elements are pushed', () => {
        const titleService = new TitleService(new TitlesConfig(), localizer.object);

        titleService.push('i18n:my-title1');
        titleService.push('i18n:my-title2');

        expect(document.title).toBe('my-title1 | my-title2');
    });

    it('should replace previous element if found', () => {
        const titleService = new TitleService(new TitlesConfig(), localizer.object);

        titleService.push('i18n:my-title1');
        titleService.push('i18n:my-title2');
        titleService.push('i18n:my-title3', 'i18n:my-title2');

        expect(document.title).toBe('my-title1 | my-title3');
    });

    it('should concatenate remainging parts if title elements are popped', () => {
        const titleService = new TitleService(new TitlesConfig(), localizer.object);

        titleService.push('i18n:my-title1');
        titleService.pop();

        expect(document.title).toBe('');
    });

    it('should prepand prefix to title', () => {
        const titleService = new TitleService(new TitlesConfig('i18n:prefix'), localizer.object);

        titleService.push('i18n:my-title');

        expect(document.title).toBe('prefix - my-title');
    });

    it('should append suffix to title', () => {
        const titleService = new TitleService(new TitlesConfig(undefined, 'i18n:suffix'), localizer.object);

        titleService.push('i18n:my-title');

        expect(document.title).toBe('my-title - suffix');
    });

    it('should use suffix if stack is empty', () => {
        const titleService = new TitleService(new TitlesConfig('i18n:prefix', 'i18n:suffix'), localizer.object);

        titleService.pop();

        expect(document.title).toBe('suffix');
    });

    it('should use suffix if stack is empty and no suffix is set', () => {
        const titleService = new TitleService(new TitlesConfig('i18n:prefix'), localizer.object);

        titleService.pop();

        expect(document.title).toBe('prefix');
    });
});

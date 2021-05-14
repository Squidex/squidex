/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, It, Mock } from 'typemoq';
import { LocalizerService } from './../../services/localizer.service';
import { TranslatePipe } from './translate.pipe';

describe('TranslatePipe', () => {
    let localizer: IMock<LocalizerService>;

    beforeEach(() => {
        localizer = Mock.ofType<LocalizerService>();
    });

    it('should invoke localizer service with string', () => {
        const pipe = new TranslatePipe(localizer.object);

        localizer.setup(x => x.getOrKey('key', It.isAny()))
            .returns(() => 'translated');

        const translation = pipe.transform('key');

        expect(translation).toEqual('translated');
    });

    it('should invoke translate method from object', () => {
        const key = {
            translate: () => {
                return 'translated';
            },
        };

        const pipe = new TranslatePipe(localizer.object);

        const translation = pipe.transform(key);

        expect(translation).toEqual('translated');
    });

    it('should return empty string if no translate method found', () => {
        const key = {
            format: () => {
                return 'translated';
            },
        };

        const pipe = new TranslatePipe(localizer.object);

        const translation = pipe.transform(key);

        expect(translation).toEqual('');
    });
});

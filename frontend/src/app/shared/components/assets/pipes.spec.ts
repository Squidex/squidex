/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { ApiUrlConfig, AssetDto, AuthService, ResourceLinkDto } from '@app/shared/internal';
import { createAsset } from '../../services/assets.service.spec';
import { AssetPreviewUrlPipe, AssetUrlPipe } from './pipes';

describe('AssetUrlPipe', () => {
    const apiUrl = new ApiUrlConfig('http://service/p/');

    it('should build url with version', () => {
        const pipe = new AssetUrlPipe(apiUrl);

        const url = pipe.transform(createAssetWithContent(), 13);

        expect(url).toEqual('http://service/p/api/assets/app/id1?version=13');
    });

    it('should not add access token to url', () => {
        const pipe = new AssetUrlPipe(apiUrl);

        const url = pipe.transform(createAssetWithContent(), 13);

        expect(url).not.toContain('access_token');
    });
});

describe('AssetPreviewUrlPipe', () => {
    const apiUrl = new ApiUrlConfig('http://service/p/');

    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType<AuthService>();
    });

    it('should add access token to url if user is authenticated', () => {
        authService.setup(x => x.user)
            .returns(() => ({ accessToken: 'token1' }) as any);

        const pipe = new AssetPreviewUrlPipe(apiUrl, authService.object);

        const url = pipe.transform(createAssetWithContent());

        expect(url).toEqual('http://service/p/api/assets/app/id1?version=1&access_token=token1');
    });
});

function createAssetWithContent() {
    const source = createAsset(1);

    return new AssetDto({
        ...source,
        _links: {
            ...source._links,
            content: new ResourceLinkDto({ method: 'GET', href: '/api/assets/app/id1' }),
        },
    } as any);
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { ProgressHttp } from 'angular-progress-http';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AssetsDto,
    AssetDto,
    AssetCreatedDto,
    AssetReplacedDto,
    AssetsService,
    AuthService,
    DateTime,
    Profile,
    UpdateAssetDto,
    Version
} from './../';

describe('AssetsService', () => {
    let authService: IMock<AuthService>;
    let assetsService: AssetsService;
    let progressHttp: IMock<ProgressHttp>;
    let version = new Version('1');

    beforeEach(() => {
        const factory = {
            create: () => {
                return <any>null;
            }
        };

        progressHttp = Mock.ofInstance(new ProgressHttp(null, null, factory, null));
        progressHttp.setup(x => x.withUploadProgressListener(It.isAny())).returns(() => <any> progressHttp.object);

        authService = Mock.ofType(AuthService);
        authService.setup(x => x.user).returns(() => new Profile(<any>{}));

        assetsService = new AssetsService(authService.object, new ApiUrlConfig('http://service/p/'), progressHttp.object);
    });

    it('should make get request to get assets', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/assets?take=17&skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: [{
                                id: 'id1',
                                created: '2016-12-12T10:10',
                                createdBy: 'Created1',
                                lastModified: '2017-12-12T10:10',
                                lastModifiedBy: 'LastModifiedBy1',
                                fileName: 'my-asset1.png',
                                fileSize: 1024,
                                fileVersion: 2,
                                mimeType: 'text/plain',
                                isImage: true,
                                pixelWidth: 1024,
                                pixelHeight: 2048,
                                version: 11
                            }, {
                                id: 'id2',
                                created: '2016-10-12T10:10',
                                createdBy: 'Created2',
                                lastModified: '2017-10-12T10:10',
                                lastModifiedBy: 'LastModifiedBy2',
                                fileName: 'my-asset2.png',
                                fileSize: 1024,
                                fileVersion: 2,
                                mimeType: 'text/plain',
                                isImage: true,
                                pixelWidth: 1024,
                                pixelHeight: 2048,
                                version: 22
                            }]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let assets: AssetsDto | null = null;

        assetsService.getAssets('my-app', 17, 13, null, null).subscribe(result => {
            assets = result;
        }).unsubscribe();

        expect(assets).toEqual(
            new AssetsDto(10, [
                new AssetDto(
                    'id1', 'Created1', 'LastModifiedBy1',
                    DateTime.parseISO_UTC('2016-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    'my-asset1.png',
                    1024, 2,
                    'text/plain',
                    true,
                    1024,
                    2048,
                    new Version('11')),
                new AssetDto('id2', 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    'my-asset2.png',
                    1024, 2,
                    'text/plain',
                    true,
                    1024,
                    2048,
                    new Version('22'))
        ]));

        authService.verifyAll();
    });

    it('should append query to find by name', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/assets?query=my-query&take=17&skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: []
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let assets: AssetsDto | null = null;

        assetsService.getAssets('my-app', 17, 13, 'my-query', null).subscribe(result => {
            assets = result;
        }).unsubscribe();

        authService.verifyAll();
    });

    it('should append mime types to find by types', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/assets?mimeTypes=text/plain,image/png&take=17&skip=13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 10,
                            items: []
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let assets: AssetsDto | null = null;

        assetsService.getAssets('my-app', 17, 13, null, ['text/plain', 'image/png']).subscribe(result => {
            assets = result;
        }).unsubscribe();

        authService.verifyAll();
    });

    it('should make post request to create asset', () => {
        progressHttp.setup(x => x.post('http://service/p/api/apps/my-app/assets', It.isAny(), It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: 'id1',
                            fileName: 'my-asset1.png',
                            fileSize: 1024,
                            fileVersion: 2,
                            mimeType: 'text/plain',
                            isImage: true,
                            pixelWidth: 1024,
                            pixelHeight: 2048,
                            version: 11
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let asset: AssetCreatedDto | null = null;

        assetsService.uploadFile('my-app', null).subscribe(result => {
            asset = <AssetCreatedDto>result;
        });

        expect(asset).toEqual(
            new AssetCreatedDto(
                'id1',
                'my-asset1.png',
                1024, 2,
                'text/plain',
                true,
                1024,
                2048,
                new Version('11')));

        authService.verifyAll();
    });

    it('should make put request to replace asset content', () => {
        progressHttp.setup(x => x.put('http://service/p/api/apps/my-app/assets/123/content', It.isAny(), It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            fileSize: 1024,
                            fileVersion: 2,
                            mimeType: 'text/plain',
                            isImage: true,
                            pixelWidth: 1024,
                            pixelHeight: 2048,
                            version: 11
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let asset: AssetReplacedDto | null = null;

        assetsService.replaceFile('my-app', '123', null, version).subscribe(result => {
            asset = <AssetReplacedDto>result;
        });

        expect(asset).toEqual(
            new AssetReplacedDto(
                1024, 2,
                'text/plain',
                true,
                1024,
                2048,
                new Version('11')));

        authService.verifyAll();
    });

    it('should make put request to update asset', () => {
        const dto = new UpdateAssetDto('My-Asset.pdf');

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/assets/123', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        assetsService.putAsset('my-app', '123', dto, version);

        authService.verifyAll();
    });

    it('should make delete request to delete asset', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/assets/123', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        assetsService.deleteAsset('my-app', '123', version);

        authService.verifyAll();
    });
});
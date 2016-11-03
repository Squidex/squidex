import * as TypeMoq from 'typemoq';

import { Observable } from 'rxjs';

import { 
    AppCreateDto,
    AppDto,
    AppsStoreService, 
    AppsService,
    AuthService
} from './../';

describe('AppsStoreService', () => {
    const oldApps = [new AppDto('id', 'name', null, null)];
    const newApp = new AppDto('id', 'new-name', null, null);

    let appsService: TypeMoq.Mock<AppsService>;
    let authService: TypeMoq.Mock<AuthService>;

    beforeEach(() => {
        appsService = TypeMoq.Mock.ofType(AppsService);
        authService = TypeMoq.Mock.ofType(AuthService);
    });

    it('should load when authenticated once', () => {
        authService.setup(x => x.isAuthenticatedChanges)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];  
        let result2: AppDto[]; 

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();
        
        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(result2).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should reload value from apps-service when called', () => {
         authService.setup(x => x.isAuthenticated)
            .returns(() => true)
            .verifiable(TypeMoq.Times.once());

         authService.setup(x => x.isAuthenticatedChanges)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.exactly(2));

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];  
        let result2: AppDto[]; 

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.reload();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(result2).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should add app to cache when created', () => {
        authService.setup(x => x.isAuthenticatedChanges)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.once());
            
        appsService.setup(x => x.postApp(TypeMoq.It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];  
        let result2: AppDto[]; 

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(new AppCreateDto('new-name')).subscribe(x => { });

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(JSON.stringify(result2)).toEqual(JSON.stringify(oldApps.concat([newApp])));

        appsService.verifyAll();
    });

    it('should not add app to cache when cache is null', () => {
        authService.setup(x => x.isAuthenticatedChanges)
            .returns(() => Observable.of(false))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.postApp(TypeMoq.It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result: AppDto[];   

        store.createApp(new AppCreateDto('new-name')).subscribe(x => { });

        store.apps.subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result).toBeNull();

        appsService.verifyAll();
    });
});
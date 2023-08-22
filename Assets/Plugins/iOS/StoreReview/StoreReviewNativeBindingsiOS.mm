#import <StoreKit/StoreKit.h>

extern "C"
{
	void RavensburgerRequestAppReview()
	{
		if([SKStoreReviewController class])
		{
			[SKStoreReviewController requestReview];
		}
	}
}

import { Elements } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import CheckoutPage from "./CheckoutPage";
import { useAppDispatch } from "../../app/store/configureStore";
import { useEffect, useState } from "react";
import agent from "../../app/api/agent";
import { setBasket } from "../basket/basketSlice";
import LoadingComponent from "../../app/layout/LoadingComponent";

const stripePromise = loadStripe(
  "pk_test_51PZ060CAjCTj89gQUw6vem4CZLW6p2BWpG2pJ5nn9lmgiwcd03miQ5xGYvE3ZbGBhHcqnXoLXthWTiEio1xuaLIs00E3sq3G8x"
);

export default function CheckoutWrapper() {
  const dispatch = useAppDispatch();
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    agent.Payments.createPaymentIntent()
      .then((basket) => {
        dispatch(setBasket(basket));
      })
      .catch((err) => console.log(err))
      .finally(() => {
        setLoading(false);
      });
  }, [dispatch]);

  if (loading) return <LoadingComponent message="Loading checkout..." />;

  return (
    <Elements stripe={stripePromise}>
      <CheckoutPage />
    </Elements>
  );
}

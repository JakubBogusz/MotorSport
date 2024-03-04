"use client";

import React, { use, useEffect } from "react";
import { useAuctionStore } from "../hooks/useAuctionStore";
import { useBidStore } from "../hooks/useBidStore";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Auction, AuctionFinished, Bid } from "@/types";
import { User } from "next-auth";
import toast from "react-hot-toast";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import { getDetailedViewData } from "../actions/auctionActions";
import AuctionFinishedToast from "../components/AuctionFinishedToast";
import { error } from "console";

type Props = {
  children: React.ReactNode;
  user: User | null;
};

export default function SignalRProvider({ children, user }: Props) {
  const [connection, setConnection] = React.useState<HubConnection | null>(
    null
  );
  const setCurrentPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);
  const apiUrl =
    process.env.NODE_ENV === "production"
      ? "https://api.carsties.store/notifications"
      : process.env.NEXT_PUBLIC_NOTIFY_URL;

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("http://localhost:6001/notifications")
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [apiUrl]);

  useEffect(() => {
    if (connection) {
      connection.start().then(() => {
        console.log("Connected to notification hub");

        connection.on("BidPlaced", (bid: Bid) => {
          if (bid.bidStatus === "Accepted") {
            setCurrentPrice(bid.auctionId, bid.amount);
          }
          addBid(bid);
        });

        connection.on("AuctionCreated", (auction: Auction) => {
          if (user?.username !== auction.seller) {
            return toast(<AuctionCreatedToast auction={auction} />, {
              duration: 10000,
            });
          }
        });

        connection.on("AuctionFinished", (finishedAuction: AuctionFinished) => {
          const auction = getDetailedViewData(finishedAuction.auctionId);
          return toast.promise(
            auction,
            {
              loading: "Loading",
              success: (auction) => (
                <AuctionFinishedToast
                  finishedAuction={finishedAuction}
                  auction={auction}
                />
              ),
              error: (err) => "Auction finished!",
            },
            { success: { duration: 10000, icon: null } }
          );
        });
      });
    }

    return () => {
      connection?.stop();
    };
  }, [connection, setCurrentPrice, addBid, user?.username]);

  return children;
}
